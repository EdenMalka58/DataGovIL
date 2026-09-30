import type { ProblemDetails } from "../types/vehicle";

export type ApiErrorKind = "notFound" | "upstream" | "network" | "http";

export class ApiError extends Error {
  readonly kind: ApiErrorKind;
  readonly status: number;
  readonly problem?: ProblemDetails;

  constructor(kind: ApiErrorKind, status: number, problem?: ProblemDetails) {
    super(problem?.detail ?? problem?.title ?? `${kind} (${status})`);
    this.name = "ApiError";
    this.kind = kind;
    this.status = status;
    this.problem = problem;
  }
}

async function readProblem(response: Response): Promise<ProblemDetails | undefined> {
  try {
    const text = await response.text();
    if (!text) return undefined;
    const parsed = JSON.parse(text) as unknown;
    return parsed && typeof parsed === "object" ? (parsed as ProblemDetails) : { detail: text };
  } catch {
    return undefined;
  }
}

/** GET a JSON resource and translate failures into a typed ApiError. */
export async function getJson<T>(url: string, signal?: AbortSignal): Promise<T> {
  let response: Response;
  try {
    response = await fetch(url, { headers: { Accept: "application/json" }, signal });
  } catch (cause) {
    if (cause instanceof DOMException && cause.name === "AbortError") throw cause;
    throw new ApiError("network", 0);
  }

  if (response.ok) return (await response.json()) as T;

  const problem = await readProblem(response);
  if (response.status === 404) throw new ApiError("notFound", 404, problem);
  if (response.status === 502 || response.status === 503 || response.status === 504) {
    throw new ApiError("upstream", response.status, problem);
  }
  throw new ApiError("http", response.status, problem);
}
