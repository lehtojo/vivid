BUCKET_SIZE = 1000
MAXIMUM_SLOT_SIZE = 10

MAP_OK = 1
MAP_FAIL = 0
MAP_KEY_DUPLICATION = -1

MapElement<K, V> {
    key: K
    value: V

    init(key: K, value: V) {
        this.key = key
        this.value = value
    }
}

MapBucket<K, V> {
    private:
    slots: Array<LinkedList<MapElement<K, V>>>
    
    public:

    init() {
        # Initialize all the slots in this bucket
        slots = Array<LinkedList<MapElement<K, V>>>(BUCKET_SIZE)

        loop (i = 0, i < BUCKET_SIZE, i++) {
            slots[i] = LinkedList<MapElement<K, V>>()
        }
    }

    add(key: K, value: V) {
        destination = 0

        if compiles { key.hash() } { destination = (key.hash() as u64) % BUCKET_SIZE }
        else { destination = (key as u64) % BUCKET_SIZE }

        slot = slots[destination]

        # The slot is not allowed to grow past a specific size
        if slot.size() >= MAXIMUM_SLOT_SIZE => MAP_FAIL

        # Two identical keys can not be stored at the same time
        loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
            if iterator.value.key == key => MAP_KEY_DUPLICATION
        }

        slot.add(MapElement<K, V>(key, value))
        => MAP_OK
    }

    get(key: K) {
        location = 0

        if compiles { key.hash() } { location = (key.hash() as u64) % BUCKET_SIZE }
        else { location = (key as u64) % BUCKET_SIZE }
        
        slot = slots[location]
        
        loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
            if iterator.value.key == key {
                => iterator.value.value
            }
        }

        => none as V
    }

    remove(key: K) {
        location = 0

        if compiles { key.hash() } { location = (key.hash() as u64) % BUCKET_SIZE }
        else { location = (key as u64) % BUCKET_SIZE }

        slot = slots[location]

        previous = 0 as LinkedListElement<MapElement<K, V>>

        loop (iterator = slot.iterator(), iterator, iterator = iterator.next) {
            if iterator.value.key == key {
                slot.remove(previous, iterator)
                => MAP_OK
            }

            previous = iterator
        }

        => MAP_FAIL
    }

    size() {
        size = 0

        loop (i = 0, i < BUCKET_SIZE, i++) {
            size += slots[i].size()
        }

        => size
    }
}

Map<K, V> {
    private:
    buckets: LinkedList<MapBucket<K, V>>

    public:
    init() {
        buckets = LinkedList<MapBucket<K, V>>()
        buckets.add(MapBucket<K, V>())
    }

    add(key: K, value: V) {
        loop (iterator = buckets.iterator(), iterator, iterator = iterator.next) {
            result = iterator.value.add(key, value)

            if result == MAP_OK => true
            else result == MAP_KEY_DUPLICATION => false
        }

        bucket = MapBucket<K, V>()
        bucket.add(key, value)

        buckets.add(bucket)

        => true
    }

    set(key: K, value: V) => add(key, value)

    get(key: K) {
        loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
            result = bucket.value.get(key)

            if result => result
        }

        => none as V
    }

    remove(key: K) {
        loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
            if bucket.value.remove(key) => true
        }

        => false
    }

    size() {
        size = 0

        loop (bucket = buckets.iterator(), bucket, bucket = bucket.next) {
            size += bucket.value.size()
        }

        => size
    }
}