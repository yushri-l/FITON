import { render, screen } from '@testing-library/react';
import App from './App';

test('renders FitON main page or login', () => {
    render(<App />);
    const el =
        screen.queryByText(/fiton/i) ||
        screen.queryByText(/login/i) ||
        screen.getByRole('main');
    expect(el).toBeInTheDocument();
});