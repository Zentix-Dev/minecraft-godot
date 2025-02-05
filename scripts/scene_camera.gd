extends Node3D

# Configuration variables
@export var rotation_speed: float = 0.005
@export var pan_speed: float = 0.01
@export var zoom_speed: float = 1.0
@export var move_speed: float = .5
@export var min_zoom: float = 1.0
@export var max_zoom: float = 50.0
@export var shift_mul: float = 1.5

@onready var camera: Camera3D = $Camera3D

func _input(event):
	# Handle mouse motion for rotation and panning
	if event is InputEventMouseMotion:
		var shift_multiplier = shift_mul if Input.is_key_pressed(KEY_SHIFT) else 1.0
		
		if Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
			rotate_camera(event.relative * shift_multiplier)
		elif Input.is_mouse_button_pressed(MOUSE_BUTTON_MIDDLE):
			pan_camera(event.relative * shift_multiplier)
	
	# Handle mouse wheel for zooming
	if event is InputEventMouseButton:
		var shift_multiplier = shift_mul if Input.is_key_pressed(KEY_SHIFT) else 1.0
		match event.button_index:
			MOUSE_BUTTON_WHEEL_UP:
				zoom_camera(1 * shift_multiplier)
			MOUSE_BUTTON_WHEEL_DOWN:
				zoom_camera(-1 * shift_multiplier)

func _process(_delta: float) -> void:
	if (Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT)):
		move_camera()

func rotate_camera(delta: Vector2):
	# Adjust camera orbit angles with clamping for pitch
	camera.rotation.y -= delta.x * rotation_speed
	camera.rotation.x = clamp(
		camera.rotation.x - delta.y * rotation_speed,
		-PI/2 + 0.1,  # Prevent flipping
		PI/2 - 0.1
	)

func move_camera():
	var forward_axis = Input.get_axis("camera_backward", "camera_forward")
	var right_axis = Input.get_axis("camera_left", "camera_right")
	var up_axis = Input.get_axis("camera_down", "camera_up")
	
	var cam_transform = camera.global_transform.basis
	var forward_vector = -cam_transform.z
	var right_vector = cam_transform.x
	var up_vector = cam_transform.y
	
	var shift_multiplier = shift_mul if Input.is_key_pressed(KEY_SHIFT) else 1.0 * move_speed
	
	camera.global_position += forward_axis * forward_vector * shift_multiplier
	camera.global_position += right_axis * right_vector * shift_multiplier
	camera.global_position += up_axis * up_vector * shift_multiplier
	
func pan_camera(delta: Vector2):
	# Translate pivot based on camera's right and up vectors
	var pan = delta * pan_speed
	var right = camera.global_transform.basis.x
	var up = camera.global_transform.basis.y
	camera.global_position += right * -pan.x - up * -pan.y

func zoom_camera(amount: float):
	# Adjust zoom distance within limits
	amount *= zoom_speed;
	var cam_transform = camera.global_transform.basis
	var forward_vector = cam_transform.z
	camera.global_position += forward_vector * amount
