extends Area2D
class_name ProjectileShot

var direction: Vector2 = Vector2.RIGHT
var speed: float = 220.0
var damage: int = 1
var life: float = 1.05
var arena_rect := Rect2(Vector2.ZERO, Vector2(320, 180))


func _ready() -> void:
	add_to_group("projectiles")
	z_index = 3
	collision_layer = 0
	collision_mask = 2
	monitoring = true
	monitorable = false
	body_entered.connect(_on_body_entered)

	var collision := CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 2.0
	collision.shape = shape
	add_child(collision)


func setup(start_position: Vector2, move_direction: Vector2, bounds: Rect2) -> void:
	global_position = start_position
	direction = move_direction.normalized() if move_direction != Vector2.ZERO else Vector2.RIGHT
	arena_rect = bounds


func _physics_process(delta: float) -> void:
	global_position += direction * speed * delta
	life -= delta
	if life <= 0.0 or not arena_rect.grow(14.0).has_point(global_position):
		queue_free()
		return
	queue_redraw()


func _on_body_entered(body: Node) -> void:
	if body and body.has_method("take_damage"):
		body.take_damage(damage, direction)
	queue_free()


func _draw() -> void:
	draw_rect(Rect2(Vector2(-2, -2), Vector2(4, 4)), Color(1.0, 0.94, 0.55), true)
	draw_rect(Rect2(-direction * 3.0 + Vector2(-1, -1), Vector2(2, 2)), Color(1.0, 0.72, 0.3), true)
