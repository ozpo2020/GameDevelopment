extends Area2D
class_name PortalGate

signal entered

var active: bool = false
var pulse: float = 0.0


func _ready() -> void:
	z_index = 1
	collision_layer = 0
	collision_mask = 1
	monitoring = false
	visible = false
	body_entered.connect(_on_body_entered)

	var collision := CollisionShape2D.new()
	var shape := CircleShape2D.new()
	shape.radius = 12.0
	collision.shape = shape
	add_child(collision)


func activate(spawn_position: Vector2) -> void:
	global_position = spawn_position
	active = true
	pulse = 0.0
	visible = true
	monitoring = true
	queue_redraw()


func deactivate() -> void:
	active = false
	pulse = 0.0
	visible = false
	monitoring = false


func _process(delta: float) -> void:
	if not active:
		return
	pulse += delta
	queue_redraw()


func _on_body_entered(body: Node) -> void:
	if active and body and body.is_in_group("player"):
		emit_signal("entered")


func _draw() -> void:
	if not active:
		return

	var outer := 10.0 + sin(pulse * 5.0) * 1.4
	draw_circle(Vector2.ZERO, outer + 5.0, Color(0.08, 0.16, 0.28, 0.45))
	for ring in range(3):
		var radius := outer - float(ring) * 2.8
		var hue := 0.46 + float(ring) * 0.06 + sin(pulse * 2.0 + float(ring)) * 0.01
		draw_arc(
			Vector2.ZERO,
			radius,
			pulse * 1.2 + float(ring),
			TAU + pulse * 1.2 + float(ring),
			32,
			Color.from_hsv(hue, 0.58, 0.95),
			2.0
		)
	draw_rect(Rect2(Vector2(-3, -3), Vector2(6, 6)), Color(1.0, 1.0, 1.0, 0.75), true)
