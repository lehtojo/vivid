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

		return r
	}
}

outline calculate(x: large, s: decimal) {
	i = Inheritor(x, s)

	console.write_line(i.a)
	console.write_line(i.b)
	console.write_line(i.c)
	console.write_line(i.d)
	console.write_line(i.x)
	console.write_line(i.y)
}

Animal {
	reaction: String

	init(reaction) {
		this.reaction = reaction
	}

	react() {
		console.write_line(reaction)
	}
}

Animal Dog {
	init() {
		Animal.init(String('Bark'))
	}
}

Animal Cat {
	init() {
		Animal.init(String('Meow'))
	}
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

	return 0
}