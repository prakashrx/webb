/**
 * Command definitions
 * With dynamic command discovery, you can invoke any registered command.
 * In the future, this could be auto-generated from your C# commands.
 */

// For now, we use a generic interface that allows any command
export interface Commands {
  [key: string]: {
    args?: any;
    returns?: any;
  };
}