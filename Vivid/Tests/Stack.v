import large_function()

export multi_return(a: large, b: large) {
	large_function()

	if a > b {
		return 1
	}
	else a < b {
		return -1
	}
	else {
		return 0
	}
}

init() {
	multi_return(10, 0)
	return 1
}