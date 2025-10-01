import { Card, CardContent, CardHeader, CardTitle } from '../ui/card';
import { Button } from '../ui/button';
import { Alert, AlertDescription } from '../ui/alert';
import { AlertCircle, Loader2, Image as ImageIcon } from 'lucide-react';

interface AvatarDisplayProps {
  isGenerating: boolean;
  generationError: string | null;
  avatarImageUrl: string | null;
  onGenerate: () => void;
  isGenerationEnabled: boolean;
}

export function AvatarDisplay({
  isGenerating,
  generationError,
  avatarImageUrl,
  onGenerate,
  isGenerationEnabled,
}: AvatarDisplayProps) {
  return (
    <Card className="bg-gray-800 border-gray-600 mt-6">
      <CardHeader>
        <CardTitle className="text-white">Your 3D Avatar</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="flex flex-col items-center justify-center p-4 rounded-lg bg-gray-900 min-h-[200px]">
          {isGenerating ? (
            <div className="flex flex-col items-center text-white">
              <Loader2 className="w-8 h-8 animate-spin mb-2" />
              <p>Generating your avatar... This may take a moment.</p>
            </div>
          ) : generationError ? (
            <Alert className="border-red-600 bg-red-900/20 text-red-300">
              <AlertCircle className="h-4 w-4 text-red-400" />
              <AlertDescription>{generationError}</AlertDescription>
            </Alert>
          ) : avatarImageUrl ? (
            <img src={avatarImageUrl} alt="Generated Avatar" className="rounded-lg max-w-full h-auto" />
          ) : (
            <div className="text-center text-gray-400">
              <ImageIcon className="w-12 h-12 mx-auto mb-2" />
              <p>Your generated avatar will appear here.</p>
            </div>
          )}
        </div>
        <div className="mt-4 flex justify-center">
          <Button
            onClick={onGenerate}
            disabled={!isGenerationEnabled || isGenerating}
            className="bg-blue-500 text-white hover:bg-blue-600 disabled:bg-gray-600"
          >
            {isGenerating ? 'Generating...' : 'Generate Avatar'}
          </Button>
        </div>
        {!isGenerationEnabled && (
            <p className="text-center text-sm text-gray-400 mt-2">
                Please save your measurements to enable avatar generation.
            </p>
        )}
      </CardContent>
    </Card>
  );
}