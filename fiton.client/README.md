# FITON Client - Frontend Application

![FITON](https://img.shields.io/badge/FITON-v1.0.0-blue?style=for-the-badge&logo=react)
![React](https://img.shields.io/badge/React-18.3.1-61DAFB?style=for-the-badge&logo=react)
![Vite](https://img.shields.io/badge/Vite-7.1.2-646CFF?style=for-the-badge&logo=vite)
![Tailwind](https://img.shields.io/badge/Tailwind-4.1.14-06B6D4?style=for-the-badge&logo=tailwindcss)

FITON Client is a modern, professional React application for personal style and measurement management. Built with cutting-edge technologies and featuring a beautiful, accessible UI design.

## ✨ Features

### 🎨 **Professional Theme System**
- Modern gradient-based design
- Fashion-focused color palette
- Glass morphism effects
- Smooth animations and transitions
- Responsive design for all devices

### 👤 **User Management**
- Secure authentication system
- User profile management
- Role-based access control

### 📏 **Measurement Tracking**
- Comprehensive body measurement input
- Real-time validation
- Progress tracking
- Data visualization

### 🎯 **Dashboard**
- Personalized welcome experience
- Statistics overview
- Quick action cards
- Professional navigation

### 🔮 **Future Ready**
- Clothes management
- Virtual try-on (coming soon)
- Style analytics (coming soon)

## 🚀 Tech Stack

- **Frontend Framework**: React 18.3.1
- **Build Tool**: Vite 7.1.2
- **Styling**: Tailwind CSS 4.1.14
- **UI Components**: Custom component library
- **Icons**: Custom SVG icon system
- **Fonts**: Inter (Google Fonts)
- **State Management**: React Context API
- **Routing**: React Router DOM 7.9.4
- **HTTP Client**: Axios 1.12.2

## 🛠️ Installation & Setup

### Prerequisites
- Node.js (v18 or higher)
- npm or yarn package manager

### Installation Steps

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd FITON/fiton.client
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm run dev
   # or
   npx vite
   ```

4. **Open in browser**
   Navigate to `http://localhost:5173`

## 📁 Project Structure

```
fiton.client/
├── src/
│   ├── components/
│   │   ├── auth/              # Authentication components
│   │   │   ├── LoginForm.jsx
│   │   │   └── RegisterForm.jsx
│   │   ├── dashboard/         # Dashboard components
│   │   │   └── Dashboard.jsx
│   │   ├── layout/           # Layout components
│   │   │   └── Layout.jsx
│   │   ├── measurements/     # Measurement components
│   │   │   └── MeasurementsPage.jsx
│   │   └── ui/              # Reusable UI components
│   │       ├── Alert.jsx
│   │       ├── Button.jsx
│   │       ├── Card.jsx
│   │       ├── Icons.jsx
│   │       ├── Input.jsx
│   │       ├── Navigation.jsx
│   │       └── Spinner.jsx
│   ├── contexts/            # React contexts
│   │   └── AuthContext.jsx
│   ├── hooks/              # Custom hooks
│   │   ├── useAuth.js
│   │   ├── useMeasurements.js
│   │   └── useUserProfile.js
│   ├── services/           # API services
│   │   └── api.js
│   ├── styles/            # Styling files
│   │   ├── index.css
│   │   └── theme.js
│   ├── types/             # Type definitions
│   │   └── index.js
│   ├── utils/            # Utility functions
│   │   └── api.js
│   ├── App.jsx           # Main app component
│   └── main.jsx          # Entry point
├── public/               # Static assets
├── tailwind.config.js    # Tailwind configuration
├── vite.config.js       # Vite configuration
└── package.json         # Dependencies and scripts
```

## 🎨 Theme System

FITON features a comprehensive theme system designed for fashion and fitness applications:

### Color Palette
- **Primary**: Blue-based palette for main actions
- **Secondary**: Cyan-based palette for secondary actions
- **Accent**: Fashion-inspired colors (purple, pink, rose)
- **Semantic**: Success, warning, error, info states

### Design Principles
- **Glass Morphism**: Semi-transparent backgrounds with blur effects
- **Gradient Aesthetics**: Fashion-forward color combinations
- **Smooth Animations**: 200ms transitions for professional feel
- **Typography**: Inter font family for optimal readability

### Component Variants
```jsx
// Card variants
<Card variant="default" />      // Standard card
<Card variant="elevated" />     // Enhanced shadow
<Card variant="gradient" />     // Gradient background
<Card variant="glass" />        // Glass morphism

// Button variants
<Button variant="primary" />    // Primary action
<Button variant="secondary" />  // Secondary action
<Button variant="gradient" />   // Fashion gradient
<Button variant="outline" />    // Outlined style
```

## 📱 Responsive Design

- **Mobile First**: Optimized for mobile devices
- **Breakpoints**: sm (640px), md (768px), lg (1024px), xl (1280px), 2xl (1536px)
- **Touch Friendly**: Proper touch targets and interactions
- **Adaptive Layouts**: Content adapts to screen size

## ♿ Accessibility

- **WCAG AA Compliant**: Meets web accessibility standards
- **Keyboard Navigation**: Full keyboard support
- **Screen Reader**: Optimized for assistive technologies
- **Focus Management**: Clear focus indicators
- **Color Contrast**: High contrast ratios
- **Reduced Motion**: Respects user motion preferences

## 🔧 Available Scripts

- `npm run dev` - Start development server
- `npm run build` - Build for production
- `npm run preview` - Preview production build
- `npm run lint` - Run ESLint

## 🌐 API Integration

The client integrates with the FITON Server API for:
- User authentication (JWT-based)
- Profile management
- Measurement CRUD operations
- Data synchronization

## 🔒 Security Features

- JWT token management
- Secure API communication
- Input validation and sanitization
- Protected routes
- Session management

## 📊 Performance

- **Bundle Size**: Optimized with Vite
- **Loading Speed**: Fast initial load
- **Animation Performance**: 60fps smooth animations
- **Code Splitting**: Lazy loading support
- **Asset Optimization**: Efficient resource loading

## 🧪 Testing

The application includes:
- Component testing setup
- Accessibility testing
- Cross-browser compatibility
- Mobile device testing

## 🔮 Upcoming Features

### Phase 1
- [ ] Dark mode support
- [ ] Advanced animations
- [ ] Enhanced accessibility features

### Phase 2
- [ ] Clothes management
- [ ] Virtual try-on functionality
- [ ] Style recommendations

### Phase 3
- [ ] Social features
- [ ] Style analytics
- [ ] AI-powered suggestions

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📝 Documentation

- [Theme System Documentation](./THEME_DOCUMENTATION.md)
- [Component API Reference](./docs/components.md)
- [Development Guidelines](./docs/development.md)

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🙋‍♂️ Support

For support and questions:
- Create an issue in the repository
- Contact the development team
- Check the documentation

---

**Built with ❤️ for your perfect fit** - FITON Team
