extends CharacterBody2D
class_name EnemyAgent

signal defeated(drop_position: Vector2, crystal_value: int)

var player_ref: PlayerController
var arena_rect := Rect2(Vector2.ZERO, Vector2(320, 180))
var active: bool = true
var max_health: int = 2
var health: int = 2
var move_speed: float = 34.0
var touch_cooldown: float = 0.0
var hurt_timer: float = 0.0
var knockback: Vector2 = Vector2.ZERO
var body_color := Color(0.47, 0.9, 0.53)
var eye_color := Color(0.07, 0.14, 0.1)
var size: float = 6.0
var collision: CollisionShape2D


func _ready() -> void:
	add_to_group("enemies")
	z_index = 4
	collision_layer = 2
	collision_mask = 1

	collision = CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = size
	collision.shape = shape
	add_child(collision)


func setup(target_player: PlayerController, stage_value: int, bounds: Rect2) -> void:
	player_ref = target_player
	arena_rect = bounds
	match stage_value:
		1:
			max_health = 2
			move_speed = 34.0
			body_color = Color(0.46, 0.89, 0.52)
			eye_color = Color(0.08, 0.18, 0.09)
		2:
			max_health = 3
			move_speed = 42.0
			body_color = Color(0.96, 0.55, 0.35)
			eye_color = Color(0.25, 0.08, 0.06)
		_:
			max_health = 4
			move_speed = 52.0
			body_color = Color(0.72, 0.45, 1.0)
			eye_color = Color(0.14, 0.08, 0.22)
	health = max_health
	size = 5.0 + float(stage_value)
	if collision and collision.shape is CircleShape2D:
		(collision.shape as CircleShape2D).radius = size
	queue_redraw()


func set_active(value: bool) -> void:
	active = value
	velocity = Vector2.ZERO


func take_damage(amount: int, push: Vector2 = Vector2.ZERO) -> bool:
	health -= amount
	hurt_timer = 0.12
	if push != Vector2.ZERO:
		knockback = push.normalized() * 95.0
	if health <= 0:
		emit_signal("defeated", global_position, 1)
		queue_free()
		return true
	queue_redraw()
	return false


func _physics_process(delta: float) -> void:
	touch_cooldown = max(0.0, touch_cooldown - delta)
	hurt_timer = max(0.0, hurt_timer - delta)
	knockback = knockback.move_toward(Vector2.ZERO, 260.0 * delta)

	var chase_dir := Vector2.ZERO
	if active and is_instance_valid(player_ref):
		var to_player := player_ref.global_position - global_position
		if to_player.length() > 0.001:
			chase_dir = to_player.normalized()
		if touch_cooldown <= 0.0 and to_player.length() < size + 8.0:
			player_ref.take_damage(1, chase_dir)
			touch_cooldown = 0.75

	velocity = chase_dir * move_speed + knockback if active else knockback
	move_and_slide()
	global_position = global_position.clamp(arena_rect.position + Vector2(8, 8), arena_rect.end - Vector2(8, 8))
	queue_redraw()


func _draw() -> void:
	var fill := body_color
	if hurt_timer > 0.0:
		fill = fill.lerp(Color(1.0, 1.0, 1.0), 0.45)

	draw_rect(
		Rect2(Vector2(-size, -size + 1.0), Vector2(size * 2.0, size * 2.0 - 1.0)),
		fill,
		true
	)
	draw_rect(
		Rect2(Vector2(-size + 1.0, -size), Vector2(size * 2.0 - 2.0, 2.0)),
		fill.darkened(0.18),
		true
	)
	draw_rect(Rect2(Vector2(-2.0, -1.0), Vector2(2.0, 2.0)), eye_color, true)
	draw_rect(Rect2(Vector2(1.0, -1.0), Vector2(2.0, 2.0)), eye_color, true)
	draw_rect(
		Rect2(Vector2(-size, size - 1.0), Vector2(size * 2.0, 2.0)),
		fill.darkened(0.25),
		true
	)
