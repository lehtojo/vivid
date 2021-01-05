Base {
	a: normal
	b: decimal

	c => a + b
	d => a * a + b * b
}

Base Inheritor {
	x: large
	y => a * get(3) + b * get(2) + c * get(1) + d

	init(x, s) {
		a = s
		b = s + 1
		
		this.x = x
	}

	get(n) {
		r = x

		loop (i = 1, i < n, i++) {
			r *= x
		}

		=> r
	}
}

outline calculate(x: large, s: decimal) {
	i = Inheritor(x, s)

	println(i.a)
	println(i.b)
	println(i.c)
	println(i.d)
	println(i.x)
	println(i.y)
}

Animal {
	reaction: String

	init(reaction) {
		this.reaction = reaction
	}

	react() => println(reaction)
}

Animal Dog {
	init() => Animal.init(String('Bark'))
}

Animal Cat {
	init() => Animal.init(String('Meow'))
}

Animals {
	static:
	dog => Dog()
	cat => Cat()
}

init() {
	calculate(-25, 1.414)
	calculate(5000, -123.456)
	calculate(-1.0, -1.0)
	calculate(10, 2.5)

	Animals.dog.react()
	Animals.cat.react()

	=> 0
}