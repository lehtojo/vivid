export register_utilization(a: large, b: large, c: large, d: large, e: large, f: large, g: large) {
	x = a + a - 1 * b * 7
	y = a - a * x * b + g
	z = x * y + g
	=> z
}

init() {
	register_utilization(1, 1, 1, 1, 1, 1, 1)
	=> 1
}