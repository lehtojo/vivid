package fi.quanfoxes;

import java.util.HashMap;

/**
 * Efficient way of storing values with names
 */
public class Bundle extends HashMap<String, Object> {

    private static final long serialVersionUID = 1L;

    /**
     * Adds a named object to the bundle
     * 
     * @param name   Name of the object
     * @param object Object to add
     * @return The previous value with same name, or null if there was no object
     *         with same name
     */
    public Object put(String name, Object object) {
        return super.put(name, object);
    }

    /**
     * Adds a named object to the bundle
     * @param name Name of the object
     * @param object Object to add
     * @return The previous value with same name, or null if there was no object with same name
     */
    public Object putString(String name, String string) {
        return super.put(name, string);
    }

    /**
     * Adds a named object to the bundle
     * @param name Name of the object
     * @param object Object to add
     * @return The previous value with same name, or null if there was no object with same name
     */
    public Object putFloat(String name, float number) {
        return super.put(name, number);
    }

    /**
     * Adds a named object to the bundle
     * @param name Name of the object
     * @param object Object to add
     * @return The previous value with same name, or null if there was no object with same name
     */
    public Object putInt(String name, int number) {
        return super.put(name, number);
    }

    /**
     * Adds a named object to the bundle
     * @param name Name of the object
     * @param object Object to add
     * @return The previous value with same name, or null if there was no object with same name
     */
    public Object putBool(String name, boolean bool) {
        return super.put(name, bool);
    }

    /**
     * Tries to find an object with the given name
     * @param name Name of the object
     * @param fallback Value to return if the object is not present
     * @return Success: Value with the given name, Failure: Fallback value
     */
    public <T> T get(String name, T fallback) {
        Object object = super.get(name);

        if (object == null) {
            return fallback;
        }

        @SuppressWarnings("unchecked")
        T item = (T)super.get(name);

        return item;
    }

    /**
     * Tries to find an object with the given name
     * @param name Name of the object
     * @param fallback Value to return if the object is not present
     * @return Success: Value with the given name, Failure: Fallback value
     */
    public String getString(String name, String fallback) {
        Object object = super.get(name);

        if (object == null || !(object instanceof String)) {
            return fallback;
        }

        return (String)object;
    }

    /**
     * Tries to find an object with the given name
     * @param name Name of the object
     * @param fallback Value to return if the object is not present
     * @return Success: Value with the given name, Failure: Fallback value
     */
    public float getFloat(String name, float fallback) {
        Object object = super.get(name);

        if (object == null || !(object instanceof Float)) {
            return fallback;
        }

        return (float)object;
    }

    /**
     * Tries to find an object with the given name
     * @param name Name of the object
     * @param fallback Value to return if the object is not present
     * @return Success: Value with the given name, Failure: Fallback value
     */
    public int getInt(String name, int fallback) {
        Object object = super.get(name);

        if (object == null || !(object instanceof Integer)) {
            return fallback;
        }

        return (int)object;
    }

    /**
     * Tries to find an object with the given name
     * @param name Name of the object
     * @param fallback Value to return if the object is not present
     * @return Success: Value with the given name, Failure: Fallback value
     */
    public boolean getBool(String name, boolean fallback) {
        Object object = super.get(name);

        if (object == null || !(object instanceof Boolean)) {
            return fallback;
        }

        return (boolean)object;
    }

    /**
     * Returns whether the bundle contains an object with the given name
     * @param name Name of the object to look for
     * @return True if the bundle contains an object with the given name, otherwise false
     */
    public boolean has(String name) {
        return containsKey(name);
    }
}