//
//  Command.h
//  InfinarioSDK
//
//  Created by Igi on 2/5/15.
//  Copyright (c) 2015 Infinario. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface Command : NSObject

- (instancetype)initWithEndpoint:(NSString *)endpoint;
- (instancetype)initWithEndpoint:(NSString *)endpoint andTimestamp:(NSNumber *)timestamp;

- (NSMutableDictionary *)getPayload;

@end
