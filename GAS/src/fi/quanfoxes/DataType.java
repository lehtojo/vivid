package fi.quanfoxes;

import java.util.Objects;

public class DataType {
    private String name;

    public DataType(String name) {
        this.name = name;
    }

    public String getName() {
        return name;
    }

    @Override
    public boolean equals(Object o) {
        if (this == o) return true;
        if (!(o instanceof DataType)) return false;
        DataType dataType = (DataType) o;
        return Objects.equals(name, dataType.name);
    }

    @Override
    public int hashCode() {
        return Objects.hash(name);
    }
}
