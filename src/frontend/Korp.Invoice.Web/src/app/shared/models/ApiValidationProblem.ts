export interface  ApiValidationProblem {
  title?: string;
  status?: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
