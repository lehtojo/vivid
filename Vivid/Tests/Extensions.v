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
	console.write_line(String('Factory created new ') + nameof(T))
	=> T()
}

init() {
	if Foo.is_larger<tiny, decimal>() {
		console.write_line('Tiny is somehow larger than decimal?')
	}
	else {
		console.write_line('Decimal seems to be larger than tiny')
	}

	factory = Factory()
	counter = factory.create<Foo.Bar.Counter>()

	loop (i = 0, i < 8, i++) {
		counter.increment()
	}

	console.write_line(counter.value)
	=> 0
}