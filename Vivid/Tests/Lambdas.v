import sqrt(x: decimal): decimal
import pow(x: decimal, y: decimal): decimal

ANIMAL_DOG = 0
ANIMAL_CAT = 1

Vector {
	x: decimal
	y: decimal

	init(x, y) {
		this.x = x
		this.y = y
	}

	invert() {
		x = -x
		y = -y
	}

	plus(other: Vector) => Vector(x + other.x, y + other.y)
	minus(other: Vector) => Vector(x - other.x, y - other.y)

	times(magnitude) => Vector(x * magnitude, y * magnitude)

	assign_plus(other: Vector) {
		x += other.x
		y += other.y
	}

	assign_times(magnitude) {
		x *= magnitude
		y *= magnitude
	}
}

Animal {
	action: (Animal) -> Vector

	type: large
	position: Vector

	interact(other: Animal) {
		move(action(other))
	}

	move(motion: Vector) {
		position += motion
	}
}

Animal Dog {
	init() {
		position = Vector(0, 0)
		type = ANIMAL_DOG

		action = (other: Animal) -> {
			dx = position.x - other.position.x
			dy = position.y - other.position.y
			distance = sqrt(dx * dx + dy * dy)
			direction = other.position - position

			if distance <= 1 and other.type == ANIMAL_CAT {
				# Run away from the cat
				direction.invert()
				direction *= 10

				bark()
				bark()
			}
			else {
				bark()
			}

			=> direction
		}
	}

	bark() => println('Bark')
}

CAT_SPEED = 2

Animal Cat {
	init() {
		position = Vector(0, 0)
		type = ANIMAL_CAT

		action = (other: Animal) -> {
			meow()

			=> (other.position - position) * CAT_SPEED
		}
	}

	meow() => println('Meow')
}

export create_default_action() {
	=> () -> println('Hi there!')
}

export execute_default_action(action: () -> _) {
	action()
}

export create_number_action() {
	=> (n: large) -> println(to_string(n))
}

export execute_number_action(action: (large) -> _, number: large) {
	action(number)
}

export create_sum_function() {
	=> (a: large, b: large) -> a + b
}

export execute_sum_function(function: (large, large) -> large, a: large, b: large) {
	=> function(a, b)
}

export create_capturing_function(x: tiny, y: small, z: normal, w: large, i: decimal) {
	=> () -> x + y + z + w + i
}

export execute_capturing_function(function: () -> decimal) {
	=> function()
}

export create_capturing_function_with_parameter(dog: Dog, cat: Cat) {
	=> (n: decimal) -> {
		h = n / 2.0

		dog.position = Vector(1, 1) * h
		cat.position = Vector(1, 1) * -h

		dog.interact(cat)
		cat.interact(dog)
	}
}

export execute_capturing_function_with_parameter(function: (decimal) -> _, distance: decimal) {
	function(distance)
}

init() {
	dog = Dog()
	cat = Cat()

	dog.position = Vector(10, 10)
	cat.position = Vector(0, 0)

	dog.interact(cat)
	cat.interact(dog)

	dog.position = Vector(0.5, 0.5)
	cat.position = Vector(0, 0)

	dog.interact(cat)
	cat.interact(dog)

	a = create_default_action()
	execute_default_action(a)

	b = create_number_action()
	execute_number_action(b, -1)

	c = create_sum_function()
	x = execute_sum_function(c, 1, 2)

	println(x)

	d = create_capturing_function(1, 2, 3, 4, 5)
	y = execute_capturing_function(d)

	println(y)

	e = create_capturing_function_with_parameter(dog, cat)
	execute_capturing_function_with_parameter(e, 1.414)
	execute_capturing_function_with_parameter(e, -0.1)

	=> 0
}