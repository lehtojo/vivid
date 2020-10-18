import sqrt(x: decimal) => decimal
import pow(x: decimal, y: decimal) => decimal

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
	action: (Animal) => Vector

	type: num
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

		action = (other: Animal) => {
			distance = sqrt(pow(position.x - other.position.x, 2.0) + pow(position.y - other.position.y, 2.0))
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

		action = (other: Animal) => {
			meow()

			=> (other.position - position) * CAT_SPEED
		}
	}

	meow() => println('Meow')
}

export create_default_action() {
	=> () => println('Hi there!')
}

export execute_default_action(action: () => _) {
	action()
}

export create_number_action() {
	=> (n: num) => printsln(to_string(n))
}

export execute_number_action(action: (num) => _, number: num) {
	action(number)
}

export create_sum_function() {
	=> (a: num, b: num) => a + b
}

export execute_sum_function(function: (num, num) => num, a: num, b: num) {
	=> function(a, b)
}

export create_capturing_function(x: tiny, y: small, z: normal, w: large, i: decimal) {
	=> () => x + y + z + w + i
}

export execute_capturing_function(function: () => decimal) {
	=> function()
}

export overloads(x: tiny) {
	=> x * 2
}

export overloads(x: num) {
	=> x - x
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

	printsln(to_string(x))

	d = create_capturing_function(1, 2, 3, 4, 5)
	y = execute_capturing_function(d)

	printsln(to_string_decimal(y))

	overloads(1)
	overloads(1 as tiny)
}