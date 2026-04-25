extends Node2D

const PlayerScript := preload("res://scripts/Player.gd")
const EnemyScript := preload("res://scripts/Enemy.gd")
const ProjectileScript := preload("res://scripts/Projectile.gd")
const CrystalScript := preload("res://scripts/Crystal.gd")
const PortalScript := preload("res://scripts/Portal.gd")
const HUDScript := preload("res://scripts/HUD.gd")

const VIEW_SIZE := Vector2(320, 180)
const ARENA_RECT := Rect2(Vector2(14, 26), Vector2(292, 140))
const MAX_STAGE := 3

enum RunState {
	PLAYING,
	VICTORY,
	GAME_OVER,
}

var player: PlayerController
var portal: PortalGate
var hud: GameHUD

var state: int = RunState.PLAYING
var stage: int = 1
var score: int = 0
var crystals_collected: int = 0
var crystals_needed: int = 0
var enemies_spawned: int = 0
var enemies_target: int = 0
var spawn_timer: float = 0.0
var stars: Array[Vector2] = []


func _ready() -> void:
	randomize()
	_build_scene()
	_generate_stars()
	queue_redraw()
	_start_new_run()


func _build_scene() -> void:
	player = PlayerScript.new() as PlayerController
	add_child(player)
	player.shoot_requested.connect(_on_player_shoot_requested)
	player.died.connect(_on_player_died)
	player.health_changed.connect(_on_player_health_changed)

	portal = PortalScript.new() as PortalGate
	add_child(portal)
	portal.entered.connect(_on_portal_entered)

	hud = HUDScript.new() as GameHUD
	add_child(hud)


func _start_new_run() -> void:
	score = 0
	_setup_stage(1, true)


func _setup_stage(next_stage: int, full_heal: bool) -> void:
	state = RunState.PLAYING
	stage = next_stage
	crystals_collected = 0
	crystals_needed = 4 + stage * 2
	enemies_spawned = 0
	enemies_target = 6 + stage * 5
	spawn_timer = 0.7

	_clear_group("enemies")
	_clear_group("projectiles")
	_clear_group("crystals")
	portal.deactivate()
	player.prepare_for_stage(ARENA_RECT, ARENA_RECT.get_center(), full_heal, 2)
	player.set_active(true)
	_set_enemies_active(true)
	hud.show_banner("STAGE %d" % stage, Color(0.97, 0.86, 0.55), 1.2)
	_refresh_hud()
	queue_redraw()


func _physics_process(delta: float) -> void:
	if state != RunState.PLAYING:
		return

	if enemies_spawned < enemies_target:
		spawn_timer -= delta
		if spawn_timer <= 0.0:
			_spawn_enemy()
			enemies_spawned += 1
			spawn_timer = max(0.35, 0.95 - float(stage) * 0.12) + randf() * 0.24

	_refresh_hud()


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventKey and event.pressed and not event.echo:
		var key_event := event as InputEventKey
		if key_event.keycode == KEY_R or key_event.keycode == KEY_ENTER:
			if state == RunState.GAME_OVER or state == RunState.VICTORY:
				_start_new_run()


func _spawn_enemy() -> void:
	var enemy := EnemyScript.new() as EnemyAgent
	enemy.global_position = _enemy_spawn_position()
	enemy.setup(player, stage, ARENA_RECT)
	enemy.defeated.connect(_on_enemy_defeated)
	add_child(enemy)


func _enemy_spawn_position() -> Vector2:
	var candidate := ARENA_RECT.get_center()
	var padding := 12.0
	for _attempt in range(8):
		var side := randi() % 4
		match side:
			0:
				candidate = Vector2(
					ARENA_RECT.position.x + padding,
					randf_range(ARENA_RECT.position.y + padding, ARENA_RECT.end.y - padding)
				)
			1:
				candidate = Vector2(
					ARENA_RECT.end.x - padding,
					randf_range(ARENA_RECT.position.y + padding, ARENA_RECT.end.y - padding)
				)
			2:
				candidate = Vector2(
					randf_range(ARENA_RECT.position.x + padding, ARENA_RECT.end.x - padding),
					ARENA_RECT.position.y + padding
				)
			_:
				candidate = Vector2(
					randf_range(ARENA_RECT.position.x + padding, ARENA_RECT.end.x - padding),
					ARENA_RECT.end.y - padding
				)

		if candidate.distance_to(player.global_position) > 72.0:
			return candidate
	return candidate


func _portal_spawn_position() -> Vector2:
	var candidate := ARENA_RECT.get_center()
	for _attempt in range(10):
		candidate = Vector2(
			randf_range(ARENA_RECT.position.x + 42.0, ARENA_RECT.end.x - 42.0),
			randf_range(ARENA_RECT.position.y + 28.0, ARENA_RECT.end.y - 28.0)
		)
		if candidate.distance_to(player.global_position) > 54.0:
			return candidate
	return candidate


func _on_player_shoot_requested(origin: Vector2, direction: Vector2) -> void:
	if state != RunState.PLAYING:
		return
	var projectile := ProjectileScript.new() as ProjectileShot
	projectile.setup(origin, direction, ARENA_RECT)
	add_child(projectile)


func _on_enemy_defeated(drop_position: Vector2, crystal_value: int) -> void:
	score += 25 * stage
	var crystal := CrystalScript.new() as CrystalPickup
	crystal.setup(drop_position, stage, crystal_value)
	crystal.collected.connect(_on_crystal_collected)
	add_child(crystal)
	_refresh_hud()


func _on_crystal_collected(value: int) -> void:
	crystals_collected += value
	score += 10 * value
	if crystals_collected % 3 == 0:
		player.restore_health(1)
	if crystals_collected >= crystals_needed and not portal.active:
		portal.activate(_portal_spawn_position())
		hud.show_banner("PORTAL ONLINE", Color(0.57, 1.0, 0.84), 2.0)
	_refresh_hud()


func _on_portal_entered() -> void:
	if state != RunState.PLAYING:
		return
	if stage >= MAX_STAGE:
		_win_run()
	else:
		_setup_stage(stage + 1, false)


func _on_player_died() -> void:
	state = RunState.GAME_OVER
	portal.deactivate()
	player.set_active(false)
	_set_enemies_active(false)
	_clear_group("projectiles")
	hud.show_sticky("SIGNAL LOST  |  PRESS R OR ENTER", Color(1.0, 0.45, 0.45))
	_refresh_hud()


func _win_run() -> void:
	state = RunState.VICTORY
	score += 250
	portal.deactivate()
	player.set_active(false)
	_set_enemies_active(false)
	_clear_group("projectiles")
	_clear_group("crystals")
	hud.show_sticky("CORE SECURED  |  PRESS R OR ENTER", Color(0.65, 1.0, 0.7))
	_refresh_hud()


func _on_player_health_changed(_current: int, _maximum: int) -> void:
	_refresh_hud()


func _set_enemies_active(active: bool) -> void:
	for node in get_tree().get_nodes_in_group("enemies"):
		if is_instance_valid(node):
			node.set_active(active)


func _clear_group(group_name: String) -> void:
	for node in get_tree().get_nodes_in_group(group_name):
		if is_instance_valid(node):
			node.queue_free()


func _refresh_hud() -> void:
	var portal_online: bool = portal.active
	hud.set_status(
		stage,
		MAX_STAGE,
		player.health,
		player.max_health,
		crystals_collected,
		crystals_needed,
		score,
		player.is_dash_ready(),
		portal_online
	)


func _generate_stars() -> void:
	stars.clear()
	for _index in range(24):
		stars.append(Vector2(randf_range(6.0, VIEW_SIZE.x - 6.0), randf_range(5.0, 20.0)))


func _draw() -> void:
	draw_rect(Rect2(Vector2.ZERO, VIEW_SIZE), Color(0.06, 0.07, 0.09), true)
	draw_rect(Rect2(Vector2(0, 0), Vector2(VIEW_SIZE.x, 22)), Color(0.09, 0.12, 0.17), true)
	for star in stars:
		draw_rect(Rect2(star, Vector2(1, 1)), Color(0.7, 0.76, 0.9), true)

	draw_rect(ARENA_RECT, Color(0.13, 0.16, 0.14), true)
	var tile_size := 16
	for y in range(int(ARENA_RECT.position.y), int(ARENA_RECT.end.y), tile_size):
		for x in range(int(ARENA_RECT.position.x), int(ARENA_RECT.end.x), tile_size):
			var x_index := int((x - int(ARENA_RECT.position.x)) / tile_size)
			var y_index := int((y - int(ARENA_RECT.position.y)) / tile_size)
			var shade := Color(0.15, 0.19, 0.16) if (x_index + y_index) % 2 == 0 else Color(0.12, 0.15, 0.13)
			draw_rect(Rect2(Vector2(x + 1, y + 1), Vector2(tile_size - 2, tile_size - 2)), shade, true)

	draw_rect(
		Rect2(ARENA_RECT.position - Vector2(3, 3), ARENA_RECT.size + Vector2(6, 6)),
		Color(0.03, 0.04, 0.05),
		false,
		3.0
	)
	draw_rect(
		Rect2(ARENA_RECT.position - Vector2(1, 1), ARENA_RECT.size + Vector2(2, 2)),
		Color(0.28, 0.34, 0.3),
		false,
		1.0
	)
