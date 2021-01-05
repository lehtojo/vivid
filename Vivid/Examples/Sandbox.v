Base {
	a: large

	foo() => a
}

Base Inheritor {
	a: large

	init() {
		a = 10
		Base.a = 10

		foo()
		Base.foo()

		Base.init()
		Base.deinit()

		deinit()
	}

	foo() => a + 1
}

init() {
	i = Inheritor()
	=> i
}