import exit(code: num)
import internal_allocate(bytes: num): link

true = 1
false = 0

none = 0

PAGE_SIZE = 1000000

Page {
    address: link
    position: num
}

Allocation {
    static:
    current: Page
}

# Safe: allocate(bytes: num) => internal_allocate(bytes)

allocate(bytes: num) {
    if Allocation.current != none and Allocation.current.position + bytes <= PAGE_SIZE {
        position = Allocation.current.position
        Allocation.current.position += bytes

        => (Allocation.current.address + position) as link
    }

    address = internal_allocate(PAGE_SIZE)

    page = internal_allocate(24) as Page
    page.address = address
    page.position = bytes

    Allocation.current = page

    => address as link
}

TYPE_DESCRIPTOR_FULLNAME_OFFSET = 0
TYPE_DESCRIPTOR_INHERITANT_SEPARATOR = 1
TYPE_DESCRIPTOR_FULLNAME_END = 2

inherits(descriptor: link, type: link) {
	x = descriptor[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link
	y = type[TYPE_DESCRIPTOR_FULLNAME_OFFSET] as link

	s = y[0]
	i = 0

	loop {
		a = x[i]
		i++

		if a == s {
			j = 1

			loop {
				a = x[i]
				b = y[j]

				i++
				j++

				if a != b and a == TYPE_DESCRIPTOR_INHERITANT_SEPARATOR and b == 0 => true
			}
		}
		else a == TYPE_DESCRIPTOR_FULLNAME_END => false
	}
}

move(source: link, offset: num, destination: link, bytes: num) {
    # Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
    buffer = allocate(bytes)
    source += offset
    copy(source, bytes, buffer)
    
    # Copy the contents of the temporary buffer to the destination
    copy(buffer, bytes, destination)

    # Delete the temporary buffer
    #deallocate(buffer, bytes)
}

move(source: link, destination: link, bytes: num) {
    # Copy the area to be moved to a temporary buffer, since moving can override the bytes to be moved
    buffer = allocate(bytes)
    copy(source, bytes, buffer)
    
    # Copy the contents of the temporary buffer to the destination
    copy(buffer, bytes, destination)

    # Delete the temporary buffer
    #deallocate(buffer, bytes)
}