Entity {
	name: link
}

Entity Banana {
	init() {
		name = 'Banana'
	}
}

Entity Apple {
	init() {
		name = 'Apple'
	}
}

Entity Player {
	item: Entity

	init() {
		name = 'John'
	}
}

Entity Enemy {
	init() {
		name = 'Enemy'
	}
}

export case_1(fruit: Entity, buyer: Entity) {
	
	if fruit == none {
		=> none as Banana
	}
	else fruit is Banana banana {
		=> banana
	}

	=> none as Banana
}

export case_2(fruit: Entity, buyer: Entity) {
	
	if fruit == none {
		=> false
	}
	else fruit is Banana banana and buyer is Player player {
		player.item = banana
		=> true
	}

	=> false
}

sum(a, b) => a + b
sub(a, b) => a - b
mul(a, b, c, d, e) => a * b * c * d * e

power(a, b) {
	if a > b {
		=> sum(a, b)
	}
	else {
		=> mul(a, a, a, a, a)
	}
}

export haha(a: large, b: large) {
	if a > b {
		=> sum(a, b)
	}
	else a < b {
		=> sum(a, b)
	}
}

init() {
	c = power(1, 2)
	=> c
}