Animal {
	energy = 100i16
	hunger = 0i8

	move() {
		--energy
		++hunger
	}
}

Fish {
	speed: small = 1
	velocity: small = 0
	weight: small = 1500

	swim(animal: Animal) {
		animal.move()
		velocity = speed
	}

	float() {
		velocity = 0
	}
}

Animal Fish Salmon {
	is_hiding = false

	init() {
		speed = 5
		weight = 5000
	}

	hide() {
		float()
		is_hiding = true
	}

	stop_hiding() {
		swim(this)
		is_hiding = false
	}
}

export get_animal() {
	=> Animal()
}

export get_fish() {
	=> Fish()
}

export get_salmon() {
	=> Salmon()
}

export animal_moves(animal: Animal) {
	animal.move()
}

export fish_moves(fish: Fish) {
	if (fish as Salmon).is_hiding == false {
		fish.swim(fish as Salmon)
	}
}

export fish_swims(animal: Animal) {
	(animal as Salmon).swim(animal)
}

export fish_stops(animal: Animal) {
	(animal as Salmon).float()
}

export fish_hides(salmon: Salmon) {
	fish_moves(salmon)
	salmon.hide()
}

export fish_stops_hiding(salmon: Salmon) {
	salmon.stop_hiding()
	salmon.swim(salmon)
}

Salmon_Gang {
	size = 1

	init(size) {
		this.size = size
	}
}

init() {
	=> true

	gang = Salmon_Gang(10)

	animal = get_animal()
	fish = get_fish()
	salmon = get_salmon()

	animal_moves(animal)
	fish_moves(fish)
	fish_swims(salmon)
	fish_stops(salmon)
	fish_hides(salmon)
	fish_stops_hiding(salmon)
}