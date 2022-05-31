namespace Foo {

	Fruit {
		name: String
	}

	Fruit Apple {
		init() {
			name = String('Apple')
		}

		init(n) {
			this.name = n
		}
	}

	namespace Bar

	Fruit Banana {
		init() {
			name = String('Banana')
		}
	}

	Factory<T> {
		new() {
			=> T(String('Factory ') + nameof(T))
		}
	}
}

init() {
	Baz.fruits()
	Baz.factory<Foo.Apple>(3)
	=> 0
}

namespace Baz

factory<T>(n) {
	factory = Foo.Bar.Factory<T>()

	loop (i = 0, i < n, i++) {
		product = factory.new()

		console.write_line(product.name)
	}
}

fruits() {
	apple = Foo.Apple()
	banana = Foo.Bar.Banana()

	console.write_line(apple.name)
	console.write_line(banana.name)
}