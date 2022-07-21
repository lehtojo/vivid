export conditionals(a, b) {
	if a >= b {
		return a
	}
	else {
		return b
	}
}

export if_statement_greater_than(a, b) {
	if a > b {
		return true
	}

	return false
}

export if_statement_greater_than_or_equal(a, b) {
	if a >= b {
		return true
	}

	return false
}

export if_statement_less_thanl(a, b) {
	if a < b {
		return true
	}

	return false
}

export if_statement_less_than_or_equal(a, b) {
	if a <= b {
		return true
	}

	return false
}

export if_statement_equals(a, b) {
	if a == b {
		return true
	}

	return false
}

export if_statement_not_equals(a, b) {
	if a != b {
		return true
	}

	return false
}

init() {
	return 1
	
	conditionals(1, 2)
}