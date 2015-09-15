# NerdAmigo Common.Configuration

The configuration project takes the IConfigurationProvider interface and provides my (currently infant, more to come on this front) implementation of a generic configuration object provider. Right now it simply looks for a file by examining the object requested for it’s namespace and class name, and de-serializes a matching file from disk.
