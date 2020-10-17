f(a, b) {
	x = a + a - 1 * b * 7
	y = a - a / x * b
	z = x * y
	=> z
}

g() {
	i = 1000

	loop() {
		i /= 2
		i = i % 10
	}

	=> i	
}

init() {
	a = 3
	b = f(1 + a, 2 * a + f(a, (a - 1) * (a + 1)))
	g()
}