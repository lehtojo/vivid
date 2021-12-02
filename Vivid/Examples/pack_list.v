###
# 1. OK
pack Foo {
	a: large
	b: large
}

init() {
	foo: Foo
	foo.a = 1
	foo.b = 2

	foos = List<Foo>()
	foos.add(foo)

	=> 0
}
###

###
# 2. Not working, because Common.GetTokens must support unnamed packs

init() {
	foo = { a: 1, b: 2 }

	foos = List<{ a: large, b: large }>()
	foos.add(foo)

	=> 0
}
###

# 3. Not working, because Common.GetTokens must support unnamed packs

init() {
	foo = { a: 1, b: 2 }
	bar = { a: -1, b: -2 }
	list = [ foo, bar ]
}

###
sum_of_packs(elements: List<{ a: large, b: large }>) {
	sum = 0.0

	loop element in elements {
		sum += element.a + element.b
	}

	=> sum
}

init() {
	sum_of_packs([{ a: 1, b: 2 }, { a: 3, b: 4 }])
}
###