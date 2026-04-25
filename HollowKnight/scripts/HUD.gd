extends CanvasLayer
class_name GameHUD

var status_label: Label
var hint_label: Label
var banner_label: Label
var banner_timer: float = 0.0
var sticky_banner: bool = false


func _ready() -> void:
	layer = 10
	var root := Control.new()
	root.set_anchors_preset(Control.PRESET_FULL_RECT)
	add_child(root)

	status_label = Label.new()
	status_label.position = Vector2(8, 4)
	status_label.size = Vector2(304, 18)
	status_label.add_theme_font_size_override("font_size", 12)
	status_label.add_theme_color_override("font_color", Color(0.92, 0.95, 1.0))
	root.add_child(status_label)

	hint_label = Label.new()
	hint_label.position = Vector2(8, 160)
	hint_label.size = Vector2(304, 16)
	hint_label.add_theme_font_size_override("font_size", 11)
	hint_label.add_theme_color_override("font_color", Color(0.75, 0.82, 0.9))
	hint_label.text = "MOVE WASD/ARROWS  SHOOT MOUSE/J  DASH SPACE"
	root.add_child(hint_label)

	banner_label = Label.new()
	banner_label.position = Vector2(24, 70)
	banner_label.size = Vector2(272, 28)
	banner_label.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	banner_label.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	banner_label.add_theme_font_size_override("font_size", 18)
	banner_label.add_theme_color_override("font_color", Color(1.0, 1.0, 1.0))
	banner_label.add_theme_constant_override("outline_size", 4)
	banner_label.add_theme_color_override("font_outline_color", Color(0.03, 0.04, 0.06))
	banner_label.visible = false
	root.add_child(banner_label)


func _process(delta: float) -> void:
	if sticky_banner or banner_timer <= 0.0:
		return
	banner_timer -= delta
	if banner_timer <= 0.0:
		banner_label.visible = false


func set_status(stage_value: int, max_stage: int, health: int, max_health: int, crystals: int, target: int, score_value: int, dash_ready: bool, portal_online: bool) -> void:
	var objective := "P OPEN" if portal_online else "C %d/%d" % [crystals, target]
	var dash_text := "READY" if dash_ready else "CHARGE"
	status_label.text = "S %d/%d  HP %02d/%02d  %s  SC %04d  DASH %s" % [
		stage_value,
		max_stage,
		health,
		max_health,
		objective,
		score_value,
		dash_text,
	]


func show_banner(text: String, color: Color, duration: float) -> void:
	sticky_banner = false
	banner_timer = duration
	banner_label.text = text
	banner_label.add_theme_color_override("font_color", color)
	banner_label.visible = true


func show_sticky(text: String, color: Color) -> void:
	sticky_banner = true
	banner_timer = 0.0
	banner_label.text = text
	banner_label.add_theme_color_override("font_color", color)
	banner_label.visible = true
