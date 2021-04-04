namespace Core {
	Object {
		name: link
		size: large

		init() {
			name = 'John'
			size = 42
		}

		init(size) {
			this.name = 'John'
			this.size = size
		}
	}
}

namespace Utility {
	Core.Object Player {
		health: large

		init() {
			Core.Object.init(30.5)
		}
	}

	Core.Object Animal {
		speed: decimal
	}
}

init() {
	object = Core.Object()
	player = Utility.Player()
	animal = Utility.Animal()
}