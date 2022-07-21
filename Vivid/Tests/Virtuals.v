InheritantOne {
	virtual foo()
}

InheritantOne VirtualTypeOne {
	override foo() {
		console.write_line(1 + 2)
	}
}

execute_virtual_type_one() {
	x = VirtualTypeOne()

	x.foo()
	(x as InheritantOne).foo()
}

InheritantTwo {
	a: large

	virtual bar()
}

InheritantTwo VirtualTypeTwo {
	b: decimal

	override bar() {
		console.write_line(a * a + b * b)
	}
}

execute_virtual_type_two() {
	x = VirtualTypeTwo()
	x.a = 7
	x.b = 42

	x.bar()
	(x as InheritantTwo).bar()
}

InheritantThree {
	b: large

	virtual baz(x: tiny, y: small): large
}

InheritantThree VirtualTypeThree {
	c: decimal

	override baz(x: tiny, y: small) {
		if x > y {
			console.write_line(x)
			return x
		}
		else y > x {
			console.write_line(y)
			return y
		}
		else {
			console.write_line(c)
			return c
		}
	}
}

execute_virtual_type_three() {
	x = VirtualTypeThree()
	x.b = 1
	x.c = 10

	console.write_line(x.baz(1, -1))
	console.write_line((x as InheritantThree).baz(255, 32767))
	console.write_line((x as InheritantThree).baz(7, 7))
}

InheritantOne InheritantTwo InheritantThree VirtualTypeFour {
	override foo() {
		a += 1
		b -= 1
	}

	override bar() {
		a *= 7
		b *= 7
	}

	override baz(x: tiny, y: small) {
		return a / b + x / y
	}
}

execute_virtual_type_four() {
	x = VirtualTypeFour()
	x.a = -6942
	x.b = 4269

	x.foo()
	x.bar()
	console.write_line(x.baz(64, 8)) # 7

	(x as InheritantOne).foo()
	(x as InheritantTwo).bar()
	console.write_line((x as InheritantThree).baz(0, 1)) # -1
}

init() {
	execute_virtual_type_one()
	execute_virtual_type_two()
	execute_virtual_type_three()
	execute_virtual_type_four()
	return 0
}