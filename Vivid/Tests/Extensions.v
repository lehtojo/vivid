namespace Foo {
	namespace Bar

	Counter {
		value: large = -1
	}
}

Factory {}

Foo.is_larger<Ta, Tb>() {
	=> sizeof(Ta) > sizeof(Tb)
}

Foo.Bar.Counter.increment() {
	=> ++value
}

Factory.create<T>() {
	println(String('Factory created new ') + nameof(T))
	=> T()
}

init() {
	if Foo.is_larger<tiny, decimal>() {
		println('Tiny is somehow larger than decimal?')
	}
	else {
		println('Decimal seems to be larger than tiny')
	}

	factory = Factory()
	counter = factory.create<Foo.Bar.Counter>()

	loop (i = 0, i < 8, i++) {
		counter.increment()
	}

	println(counter.value)
	=> 0
}