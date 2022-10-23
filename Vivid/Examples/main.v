export allocate(bytes: i64) {}
export deallocate(address: link) {}
export internal_is(a: link, b: link) {}

init() {
	a = 1
	b = 2
	return a * 2 - b
}