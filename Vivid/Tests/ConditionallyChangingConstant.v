export conditionally_changing_constant_with_if_statement(a, b) {
	constant = 7

	if a > b {
		constant = a
	}

	=> a + constant
}

export conditionally_changing_constant_with_loop_statement(a, b) {
	constant = 100

	loop (a, a < b, ++a) {
		constant += 1
	}

	=> b * constant
}

init() {
	=> 1
	
	# Dummy for type resolvation
	conditionally_changing_constant_with_if_statement(1, 1)
	conditionally_changing_constant_with_loop_statement(1, 1)
}