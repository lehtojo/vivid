pack Bar {
	a: large
	b: large
}

pack Foo {
	a: Bar
	b: Bar
	c: Bar
	d: Bar
}

export case_1(bar: Bar) {
	other = bar
	bar.a = 1
	=> other
}

export case_2() {
	foo = Foo()
	=> foo
}

init() => true