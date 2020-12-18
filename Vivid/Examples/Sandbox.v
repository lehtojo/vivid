A {
	x: large

	change() {
		x = -1
		=> x
	}
}

init() {
	buffer = allocate<A>(10)
	=> buffer > 0
}