import { FormGroup } from '@angular/forms';
import { ApiValidationProblem } from '../models/ApiValidationProblem';


export function applyApiValidationErrors(
  form: FormGroup,
  problem: ApiValidationProblem): boolean {
  if (!problem.errors)
    return false;


  let hasMappedError = false;

  for (const [propertyName, messages] of Object.entries(problem.errors)) {
    const controlName =
      propertyName.charAt(0).toLowerCase() +
      propertyName.slice(1);

    const control = form.get(controlName);

    if (!control)
      continue;


    control.setErrors({...control.errors,server: messages.join(' ')});
    control.markAsTouched();
    hasMappedError = true;
  }

  return hasMappedError;
}
export type { ApiValidationProblem };

