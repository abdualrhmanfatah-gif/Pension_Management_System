import type { Meta, StoryObj } from "@storybook/react";
import { fn } from "@storybook/test";
import { Briefcase,FileText, Info, User } from "lucide-react";
import { useState } from "react";
import { useForm } from "react-hook-form";

import { BaseFormDialog } from "../../BaseFormDialog";
import { Button } from "../../Button";
import { Input, Textarea } from "../../Input";
import { FileField } from "../FileField";
import { FormSection } from "../FormSection";
import { TabbedFormSection } from "../TabbedFormSection";

export const meta: Meta<typeof BaseFormDialog> = {
  title: "UI/Dialogs/DemoDialog",
  component: BaseFormDialog,
  parameters: {
    layout: "centered",
  },
};



const DialogWrapper = (props: Record<string, unknown>) => {
  const [isOpen, setIsOpen] = useState(false);
  const { control, handleSubmit, reset, formState: { isDirty, errors } } = useForm({
    defaultValues: {
      fullName: "",
      nationalId: "",
      notes: "",
      profilePic: null,
      jobTitle: "",
      department: ""
    }
  });

  const onSubmit = (data: Record<string, unknown>) => {
    // submitted
    fn()(data);
    setIsOpen(false);
  };

  const tabs = [
    {
      id: "basic",
      label: "البيانات الأساسية",
      icon: <User className="w-4 h-4" />,
      content: (
        <div className="space-y-6">
          <FormSection title="المعلومات الشخصية" icon={User}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input 
                label="الاسم الكامل" 
                placeholder="أدخل الاسم الرباعي" 
                {...control.register("fullName", { required: "هذا الحقل مطلوب" })}
                error={errors.fullName?.message as string}
              />
              <Input 
                label="الرقم الوطني" 
                placeholder="1010XXXXXX" 
                {...control.register("nationalId", { required: "هذا الحقل مطلوب" })}
                error={errors.nationalId?.message as string}
              />
            </div>
          </FormSection>
          
          <FormSection title="معلومات العمل" icon={Briefcase}>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <Input 
                label="المسمى الوظيفي" 
                placeholder="مثال: مبرمج" 
                {...control.register("jobTitle")}
              />
              <Input 
                label="الإدارة" 
                placeholder="إدارة النظم" 
                {...control.register("department")}
              />
            </div>
          </FormSection>
        </div>
      )
    },
    {
      id: "attachments",
      label: "المرفقات والملاحظات",
      icon: <FileText className="w-4 h-4" />,
      content: (
        <div className="space-y-6">
          <FormSection title="الصورة الشخصية" icon={User}>
             <FileField 
               name="profilePic" 
               control={control} 
               label="صورة الموظف" 
               hint="يرجى رفع صورة واضحة بدقة عالية"
             />
          </FormSection>
          
          <FormSection title="ملاحظات إضافية" icon={Info}>
            <Textarea 
              label="الملاحظات" 
              placeholder="أدخل أي ملاحظات إضافية هنا..." 
              {...control.register("notes")}
              className="min-h-[120px]"
            />
          </FormSection>
        </div>
      )
    }
  ];

  return (
    <div className="p-20">
      <Button onClick={() => setIsOpen(true)}>فتح النموذج التجريبي</Button>
      
      <BaseFormDialog
        {...props}
        isOpen={isOpen}
        onClose={() => setIsOpen(false)}
        onSubmit={handleSubmit(onSubmit)}
        isDirty={isDirty}
        reset={reset}
        formErrors={errors}
        title="إضافة موظف جديد"
        description="يرجى تعبئة كافة البيانات المطلوبة لإضافة الموظف إلى النظام."
        size="2xl"
        showGlobalError
        resetOnOpen
      >
        <TabbedFormSection tabs={tabs} isDialogOpen={isOpen} />
      </BaseFormDialog>
    </div>
  );
};

export const FullFeatureDemo: StoryObj = {
  render: (args) => <DialogWrapper {...args} />,
};
