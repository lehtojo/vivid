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
			distance = sqrt(pow(this.position.x - other.position.x, 2.0) + pow(this.position.y - other.position.y, 2.0))
			direction = other.position - this.position

			if distance <= 1 and other.type == ANIMAL_CAT {
				# Run away from the cat
				direction.invert()
				direction *= 10

				this.bark()
				this.bark()
			}
			else {
				this.bark()
			}

			=> direction
		}
	}

	bark() {
		println('Bark')
	}
}

CAT_SPEED = 2

Animal Cat {
	init() {
		position = Vector(0, 0)
		type = ANIMAL_CAT

		action = (other: Animal) => {
			this.meow()

			=> (other.position - this.position) * CAT_SPEED
		}
	}

	meow() {
		println('Meow')
	}
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
}