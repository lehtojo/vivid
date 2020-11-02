TYPE_DESCRIPTOR_NAME_OFFSET = 0
TYPE_DESCRIPTOR_SIZE_OFFSET = 8
TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET = 16
TYPE_DESCRIPTOR_SUPERTYPES_FIRST = 24
TYPE_DESCRIPTOR_SUPERTYPE_STRIDE = 8

TypeDescriptor {
    private:
    address: link

    get_supertype_count() {
        => address[TYPE_DESCRIPTOR_SUPERTYPES_COUNT_OFFSET] as normal
    }

    public:

    init(address: link) {
        this.address = (address[0] as link)[0] as link
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
            supertype = address[TYPE_DESCRIPTOR_SUPERTYPES_FIRST + i * TYPE_DESCRIPTOR_SUPERTYPE_STRIDE] as link
            supertypes[i] = TypeDescriptor(supertype)
        }

        => supertypes
    }
}