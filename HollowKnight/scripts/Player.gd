extends CharacterBody2D
class_name PlayerController

signal shoot_requested(origin: Vector2, direction: Vector2)
signal died
signal health_changed(current: int, maximum: int)

const MOVE_SPEED := 80.0
const DASH_SPEED := 190.0
const DASH_TIME := 0.16
const DASH_COOLDOWN := 0.9
const SHOT_DELAY := 0.16

var arena_rect := Rect2(Vector2.ZERO, Vector2(320, 180))
var max_health: int = 6
var health: int = 6
var facing: Vector2 = Vector2.RIGHT
var shot_cooldown: float = 0.0
var dash_cooldown: float = 0.0
var dash_timer: float = 0.0
var invuln_timer: float = 0.0
var knockback: Vector2 = Vector2.ZERO
var active: bool = true


func _ready() -> void:
	add_to_group("player")
	z_index = 6
	collision_layer = 1
	collision_mask = 2

	var collision := CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 5.0
	collision.shape = shape
	add_child(collision)


func prepare_for_stage(bounds: Rect2, spawn_position: Vector2, full_heal: bool, heal_amount: int) -> void:
	arena_rect = bounds
	global_position = spawn_position
	facing = Vector2.RIGHT
	velocity = Vector2.ZERO
	knockback = Vector2.ZERO
	shot_cooldown = 0.0
	dash_cooldown = 0.0
	dash_timer = 0.0
	invuln_timer = 0.0
	if full_heal:
		health = max_health
	else:
		health = min(max_health, health + heal_amount)
	emit_signal("health_changed", health, max_health)
	queue_redraw()


func set_active(value: bool) -> void:
	active = value
	velocity = Vector2.ZERO
	queue_redraw()


func is_dash_ready() -> bool:
	return dash_cooldown <= 0.0


func restore_health(amount: int) -> void:
	if amount <= 0 or health <= 0:
		return
	var old_health := health
	health = min(max_health, health + amount)
	if health != old_health:
		emit_signal("health_changed", health, max_health)


func take_damage(amount: int, from_direction: Vector2 = Vector2.ZERO) -> void:
	if invuln_timer > 0.0 or health <= 0:
		return
	health = max(health - amount, 0)
	invuln_timer = 0.7
	if from_direction != Vector2.ZERO:
		knockback = from_direction.normalized() * 120.0
	emit_signal("health_changed", health, max_health)
	if health <= 0:
		active = false
		emit_signal("died")
	queue_redraw()


func _physics_process(delta: float) -> void:
	shot_cooldown = max(0.0, shot_cooldown - delta)
	dash_cooldown = max(0.0, dash_cooldown - delta)
	dash_timer = max(0.0, dash_timer - delta)
	invuln_timer = max(0.0, invuln_timer - delta)
	knockback = knockback.move_toward(Vector2.ZERO, 360.0 * delta)

	if not active:
		velocity = Vector2.ZERO
		return

	var input_dir := _get_move_input()
	if input_dir != Vector2.ZERO:
		facing = input_dir

	if input_dir != Vector2.ZERO and dash_cooldown <= 0.0 and Input.is_physical_key_pressed(KEY_SPACE):
		dash_timer = DASH_TIME
		dash_cooldown = DASH_COOLDOWN
		invuln_timer = max(invuln_timer, 0.2)

	var current_speed := DASH_SPEED if dash_timer > 0.0 else MOVE_SPEED
	velocity = input_dir * current_speed + knockback
	move_and_slide()
	global_position = global_position.clamp(arena_rect.position + Vector2(8, 8), arena_rect.end - Vector2(8, 8))

	_handle_shoot(input_dir)
	queue_redraw()


func _get_move_input() -> Vector2:
	var move_x := 0.0
	var move_y := 0.0
	if Input.is_physical_key_pressed(KEY_A) or Input.is_physical_key_pressed(KEY_LEFT):
		move_x -= 1.0
	if Input.is_physical_key_pressed(KEY_D) or Input.is_physical_key_pressed(KEY_RIGHT):
		move_x += 1.0
	if Input.is_physical_key_pressed(KEY_W) or Input.is_physical_key_pressed(KEY_UP):
		move_y -= 1.0
	if Input.is_physical_key_pressed(KEY_S) or Input.is_physical_key_pressed(KEY_DOWN):
		move_y += 1.0
	return Vector2(move_x, move_y).normalized()


func _handle_shoot(move_input: Vector2) -> void:
	var wants_mouse_shot := Input.is_mouse_button_pressed(MOUSE_BUTTON_LEFT)
	var wants_key_shot := Input.is_physical_key_pressed(KEY_J)
	if shot_cooldown > 0.0 or (not wants_mouse_shot and not wants_key_shot):
		return

	var shot_dir := facing
	if wants_mouse_shot:
		var mouse_delta := get_global_mouse_position() - global_position
		if mouse_delta.length() > 4.0:
			shot_dir = mouse_delta.normalized()
	elif move_input != Vector2.ZERO:
		shot_dir = move_input

	facing = shot_dir
	shot_cooldown = SHOT_DELAY
	emit_signal("shoot_requested", global_position + shot_dir * 9.0, shot_dir)


func _draw() -> void:
	if invuln_timer > 0.0 and int(invuln_timer * 24.0) % 2 == 0:
		return

	draw_rect(Rect2(Vector2(-5, 5), Vector2(10, 2)), Color(0.0, 0.0, 0.0, 0.28), true)

	var body_color := Color(0.28, 0.92, 0.98)
	if dash_timer > 0.0:
		body_color = Color(0.9, 1.0, 1.0)

	draw_rect(Rect2(Vector2(-4, -5), Vector2(8, 10)), body_color, true)
	draw_rect(Rect2(Vector2(-2, -7), Vector2(4, 2)), Color(0.13, 0.2, 0.32), true)
	draw_rect(Rect2(Vector2(-5, -4), Vector2(1, 8)), Color(0.18, 0.25, 0.34), true)
	draw_rect(Rect2(Vector2(4, -4), Vector2(1, 8)), Color(0.18, 0.25, 0.34), true)

	var eye_x := 1.0 if facing.x >= 0.0 else -3.0
	draw_rect(Rect2(Vector2(eye_x, -2), Vector2(2, 2)), Color(0.04, 0.07, 0.12), true)

	var muzzle := facing.normalized() * 5.0
	draw_rect(Rect2(muzzle + Vector2(-1, -1), Vector2(2, 2)), Color(1.0, 0.96, 0.7), true)
