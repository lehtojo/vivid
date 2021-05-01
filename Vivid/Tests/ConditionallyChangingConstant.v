export conditionally_changing_constant_with_if_statement(a: large, b: large) {
	c = 7

	if a > b {
		c = a
	}

	=> a + c
}

export conditionally_changing_constant_with_loop_statement(a: large, b: large) {
	c = 100

	loop (a < b, ++a) {
		c += 1
	}

	=> b * c
}

init() {
	=> 1
}