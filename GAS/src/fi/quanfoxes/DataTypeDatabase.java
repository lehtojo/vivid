package fi.quanfoxes;

import java.util.ArrayList;

public class DataTypeDatabase {
    private static ArrayList<DataType> dataTypes = new ArrayList<>();

    public static void add (DataType dataType) {
        dataTypes.add(dataType);
    }

    public static boolean exists (String name) {
        return dataTypes.stream().anyMatch(t -> t.getName().equals(name));
    }

    public static DataType get (String name) {
        return dataTypes.stream().filter(t -> t.getName().equals(name)).findFirst().get();
    }
}
