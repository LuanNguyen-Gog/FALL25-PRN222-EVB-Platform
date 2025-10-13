import React from "react";
import { render, screen } from "@testing-library/react";
import Page from "@/app/page";

describe("Home page", () => {
  it("renders the Next.js heading", () => {
    render(<Page />);
    expect(
      screen.getByRole("heading", { name: /next\.js/i })
    ).toBeInTheDocument();
  });
});


