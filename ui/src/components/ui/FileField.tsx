import { useRef } from "react";
import type { Control, FieldValues, Path } from "react-hook-form";
import { Controller } from "react-hook-form";

import type { FileUploaderProps } from "@/components/ui/FileUploader";
import { FileUploader } from "@/components/ui/FileUploader";
import { FormField } from "@/components/ui/FormField";

interface FileFieldProps<T extends FieldValues> extends Omit<FileUploaderProps, "value" | "onChange"> {
  name: Path<T>;
  control: Control<T>;
  label: string;
  required?: boolean;
}

/**
 * FileField: A controlled wrapper for FileUploader with robust keyboard access.
 * Integrates seamlessly with react-hook-form and triggers file selection via Enter key.
 */
export function FileField<T extends FieldValues>({
  name,
  control,
  label,
  required,
  ...props
}: FileFieldProps<T>) {
  const containerRef = useRef<HTMLDivElement>(null);

  return (
    <FormField 
      label={label} 
      required={required} 
      error={control._formState.errors[name]?.message as string}
    >
      <Controller
        name={name}
        control={control}
        render={({ field }) => (
          <div 
            ref={containerRef}
            className="focus-within:ring-2 focus-within:ring-primary/50 focus-within:ring-offset-2 rounded-2xl outline-none transition-all"
            tabIndex={0}
            onKeyDown={(e) => {
              // Support Enter key to trigger file selection if the wrapper is focused
              if (e.key === "Enter" && !props.disabled) {
                const target = e.target as HTMLElement;
                // Avoid double trigger if a button inside is already focused
                if (!target.closest('button')) {
                    e.preventDefault(); // Prevent accidental form submission
                    const input = containerRef.current?.querySelector('input[type="file"]') as HTMLInputElement;
                    if (input) input.click();
                }
              }
            }}
          >
            <FileUploader
              {...props}
              value={field.value}
              onChange={field.onChange}
              error={control._formState.errors[name]?.message as string}
            />
          </div>
        )}
      />
    </FormField>
  );
}
