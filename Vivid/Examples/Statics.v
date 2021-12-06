Foo {
	a: large = 1
	b: large = 2
	static c: large = 7 + 42
	static d: large = -7 + 42
}

init() {
	println((Foo.c + Foo.d) / 2)
}

###
export internal_init() {
	println((Foo.c + Foo.d) / 2)
	init()
}
###