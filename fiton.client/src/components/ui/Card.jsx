import React from 'react';

export const Card = ({ children, className = '', variant = 'default', ...props }) => {
  const variants = {
    default: 'bg-white border border-gray-200 shadow-card hover:shadow-hover',
    elevated: 'bg-white border border-gray-200 shadow-lg hover:shadow-xl',
    gradient: 'bg-gradient-fashion text-white border-0 shadow-lg hover:shadow-xl',
    glass: 'bg-white/80 backdrop-blur-sm border border-gray-200/50 shadow-card hover:shadow-hover',
  };

  return (
    <div
      className={`rounded-xl transition-all duration-300 ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </div>
  );
};

export const CardHeader = ({ children, className = '', ...props }) => {
  return (
    <div
      className={`px-6 py-4 border-b border-gray-100 ${className}`}
      {...props}
    >
      {children}
    </div>
  );
};

export const CardContent = ({ children, className = '', ...props }) => {
  return (
    <div className={`px-6 py-4 ${className}`} {...props}>
      {children}
    </div>
  );
};

export const CardFooter = ({ children, className = '', ...props }) => {
  return (
    <div
      className={`px-6 py-4 border-t border-gray-100 ${className}`}
      {...props}
    >
      {children}
    </div>
  );
};