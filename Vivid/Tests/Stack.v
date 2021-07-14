import large_function()

export multi_return(a: large, b: large) {
	large_function()

	if a > b {
		=> 1
	}
	else a < b {
		=> -1
	}
	else {
		=> 0
	}
}

init() {
	multi_return(10, 0)
	=> 1
}