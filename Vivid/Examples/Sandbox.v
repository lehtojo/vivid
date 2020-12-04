Goo {
	x: num
	y: num

	init() {
		x = 1
		y = -1
	}

	foo(c) {
		=> x + y + c
	}
}

init() {
	goo = Goo()
	g = 3.141

	a = 1
	b = 2

	loop (i = 0, i < 10, i++) {
		a++
	}

	b = goo.foo(a + b)

	if a > b {
		a = 10
	}
	else {
		b = 5
	}

	=> a + b
}