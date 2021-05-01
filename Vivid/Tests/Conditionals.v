export conditionals(a, b) {
	if a >= b {
		=> a
	}
	else {
		=> b
	}
}

export if_statement_greater_than(a, b) {
	if a > b {
		=> true
	}

	=> false
}

export if_statement_greater_than_or_equal(a, b) {
	if a >= b {
		=> true
	}

	=> false
}

export if_statement_less_thanl(a, b) {
	if a < b {
		=> true
	}

	=> false
}

export if_statement_less_than_or_equal(a, b) {
	if a <= b {
		=> true
	}

	=> false
}

export if_statement_equals(a, b) {
	if a == b {
		=> true
	}

	=> false
}

export if_statement_not_equals(a, b) {
	if a != b {
		=> true
	}

	=> false
}

init() {
	=> 1
	
	conditionals(1, 2)
}