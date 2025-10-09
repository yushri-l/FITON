import React from 'react';

export const Input = ({
  label,
  error,
  className = '',
  type = 'text',
  required = false,
  icon,
  ...props
}) => {
  // Handle numeric input restrictions
  const handleInput = (e) => {
    if (type === 'number') {
      // Allow only numeric characters, decimal point, and backspace
      const value = e.target.value;
      if (value !== '' && !/^\d*\.?\d*$/.test(value)) {
        e.preventDefault();
        return;
      }
    }
    
    // Call the original onChange if provided
    if (props.onChange) {
      props.onChange(e);
    }
  };

  const inputClasses = `
    w-full px-4 py-3 border rounded-xl transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent
    ${error 
      ? 'border-red-300 focus:ring-red-500 bg-red-50' 
      : 'border-gray-300 focus:ring-blue-500 bg-white hover:border-gray-400'
    }
    ${icon ? 'pl-11' : ''}
    ${className}
  `;

  return (
    <div className="w-full">
      {label && (
        <label className="block text-sm font-medium text-gray-700 mb-2">
          {label}
          {required && <span className="text-red-500 ml-1">*</span>}
        </label>
      )}
      <div className="relative">
        {icon && (
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
            <div className="h-5 w-5 text-gray-400">
              {icon}
            </div>
          </div>
        )}
        <input
          className={inputClasses}
          type={type}
          required={required}
          {...props}
          onChange={handleInput}
        />
      </div>
      {error && (
        <p className="text-red-600 text-sm mt-1 flex items-center">
          <svg className="w-4 h-4 mr-1" fill="currentColor" viewBox="0 0 20 20">
            <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
          </svg>
          {error}
        </p>
      )}
    </div>
  );
};