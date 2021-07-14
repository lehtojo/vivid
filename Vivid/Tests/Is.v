Entity {}

Entity Person {
	name: String
	skill: large
	likes_driving: bool
	likes_riding: bool

	init(name: link, skill: large, likes_driving: bool, likes_riding: bool) {
		this.name = String(name)
		this.skill = skill
		this.likes_driving = likes_driving
		this.likes_riding = likes_riding
	}
}

Usable {
	virtual likes(entity: Entity): bool
}

Usable Vehicle {
	speed: decimal
	weight: large
	acceleration: decimal
	passengers: small

	time(distance: decimal) => sqrt(2 * distance / acceleration)

	virtual skill(): large
	virtual reliability(): large
}

Drivable {}
Ridable {}

Ridable Vehicle Pig {

	init() {
		speed = 7
		weight = 100
		acceleration = 3
		passengers = 1
	}

	override skill() => 1
	override reliability() => -1

	override likes(entity: Entity) {
		=> entity is Person person and person.likes_riding
	}
}

Drivable Vehicle Car {

	init() {
		speed = 55
		weight = 1500
		acceleration = 5.555
		passengers = 5
	}

	override skill() => 10
	override reliability() => 100

	override likes(entity: Entity) {
		=> entity is Person person and person.likes_driving
	}
}

Usable Entity Banana {
	override likes(entity: Entity) => true
}

Drivable Vehicle Bus {
	init() {
		speed = 40
		weight = 4000
		acceleration = 2.5
		passengers = 40
	}

	override skill() => 40
	override reliability() => 100

	override likes(entity: Entity) {
		=> entity is Person person and person.likes_driving
	}
}

export can_use(entity: Entity, usable: Usable) {
	if !usable.likes(entity) {
		=> false
	}
	else usable is Vehicle vehicle and entity is Person person {
		=> person.skill >= vehicle.skill()
	}

	=> false
}

export get_reliable_vehicles(usables: Array<Usable>, min_reliability: large) {
	vehicles = List<Vehicle>()

	loop (i = 0, i < usables.count, i++) {
		if usables[i] is Vehicle {
			vehicles.add(usables[i] as Vehicle)
		}
	}

	loop (i = vehicles.size() - 1, i >= 0, i--) {
		if vehicles[i].reliability() < min_reliability {
			vehicles.remove_at(i)
		}
	}

	=> vehicles
}

export choose_vehicle(entity: Entity, vehicles: List<Vehicle>, distance: large) {
	=> choose_vehicle(entity, vehicles, distance as decimal)
}

export choose_vehicle(entity: Entity, vehicles: List<Vehicle>, distance: decimal) {
	if entity is Person person and person.name == 'Steve' {
		=> Pig() as Vehicle
	}

	chosen_vehicle = vehicles[0]
	minimum_time = vehicles[0].time(distance)

	loop (i = 1, i < vehicles.size(), i++) {
		vehicle = vehicles[i]
		time = vehicle.time(distance)

		if time < minimum_time {
			chosen_vehicle = vehicle
			minimum_time = time
		}
	}

	=> chosen_vehicle
}

export create_pig() => Pig()
export create_bus() => Bus()
export create_car() => Car()
export create_banana() => Banana()

export create_john() => Person('John', 10, true, false)
export create_max() => Person('Max', 7, true, true)
export create_gabe() => Person('Gabe', 50, true, false)
export create_steve() => Person('Steve', 1, false, true)

export create_array(size: large) => Array<Usable>(size)
export set(array: Array<Usable>, usable: Usable, i: large) {
	array[i] = usable
}

export is_pig(vehicle: Vehicle) => vehicle is Pig

init() {
	pig = create_pig()
	bus = create_bus()
	car = create_car()
	banana = create_banana()

	john = create_john()
	max = create_max()
	gabe = create_gabe()
	steve = create_steve()

	array = create_array(4)
	set(array, pig, 0)
	set(array, bus, 1)
	set(array, car, 2)
	set(array, banana, 3)

	get_reliable_vehicles(array, -1000000)
	=> true
}