TYPE_DESCRIPTOR_NAME_OFFSET = 0
TYPE_DESCRIPTOR_SIZE_OFFSET = 1
TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET = 2
TYPE_DESCRIPTOR_SUPERTYPES_FIRST = 3

TypeDescriptor {
	private:
	address: link

	get_supertype_count() {
		=> address[TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET] as normal
	}

	public:

	init(address: link) {
		this.address = (address as link<link<link<large>>>)[0][0]
	}

	equals(other: TypeDescriptor) => equals(other.address)

	equals(other: link) {
		if other == address {
			=> true
		}

		supertypes = supertypes()
		count = supertypes.count

		loop (i = 0, i < count, i++) {
			if supertypes[i].address == other {
				=> true
			}
		}

		=> false
	}

	name() => String(address[TYPE_DESCRIPTOR_NAME_OFFSET] as link)

	size() => address[TYPE_DESCRIPTOR_SIZE_OFFSET] as normal

	supertypes() {
		count = get_supertype_count()
		supertypes = Array<TypeDescriptor>(count)

		loop (i = 0, i < count, i++) {
			supertype = address[TYPE_DESCRIPTOR_SUPERTYPES_FIRST + i] as link
			supertypes[i] = TypeDescriptor(supertype)
		}

		=> supertypes
	}
}

export typeof(object) => TypeDescriptor(object as link)

export internal_sizeof(object) => TypeDescriptor(object as link).size()
export internal_nameof(object) => TypeDescriptor(object as link).name()