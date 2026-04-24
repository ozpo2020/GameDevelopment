extends Area2D
class_name CrystalPickup

signal collected(value: int)

var value: int = 1
var tier: int = 1
var base_position: Vector2 = Vector2.ZERO
var float_time: float = 0.0
var phase: float = 0.0


func _ready() -> void:
	add_to_group("crystals")
	z_index = 2
	collision_layer = 0
	collision_mask = 1
	monitoring = true
	monitorable = false
	body_entered.connect(_on_body_entered)

	var collision := CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 5.0
	collision.shape = shape
	add_child(collision)


func setup(spawn_position: Vector2, stage_value: int, crystal_value: int) -> void:
	base_position = spawn_position
	global_position = spawn_position
	tier = stage_value
	value = crystal_value
	phase = randf() * TAU
	queue_redraw()


func _process(delta: float) -> void:
	float_time += delta
	global_position = base_position + Vector2(0.0, sin(float_time * 4.5 + phase) * 2.2)
	queue_redraw()


func _on_body_entered(body: Node) -> void:
	if body and body.is_in_group("player"):
		emit_signal("collected", value)
		queue_free()


func _draw() -> void:
	var fill := Color(0.3, 0.96, 0.84)
	match tier:
		2:
			fill = Color(1.0, 0.78, 0.42)
		3:
			fill = Color(0.8, 0.54, 1.0)

	var points := PackedVector2Array([
		Vector2(0, -5),
		Vector2(4, 0),
		Vector2(0, 5),
		Vector2(-4, 0),
	])
	draw_colored_polygon(points, fill)
	draw_polyline(PackedVector2Array([points[0], points[1], points[2], points[3], points[0]]), fill.darkened(0.28), 1.0)
	draw_rect(Rect2(Vector2(-1, -2), Vector2(2, 2)), Color(1.0, 1.0, 1.0, 0.85), true)
